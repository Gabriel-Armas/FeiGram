from datetime import datetime, timedelta, timezone
from fastapi import APIRouter, HTTPException, Depends, File, UploadFile
from src.db import posts_collection, get_next_post_id
from src.schemas import PostCreate
from bson.objectid import ObjectId
from src.auth import get_current_user
from src.CommentCountRpcClient import CommentCountRpcClient
from src.CommentListRpcClient import CommentListRpcClient
from src.LikesCountRpcClient import LikesCountRpcClient
import cloudinary.uploader
import pytz

router = APIRouter()

def convert_to_local(utc_dt):
    local_tz = pytz.timezone("America/Mexico_City")
    return utc_dt.astimezone(local_tz)

@router.post("/posts")
def create_post(post: PostCreate, user_id: str = Depends(get_current_user)):
    print(post.fechaPublicacion)
    local_dt = post.fechaPublicacion.astimezone(pytz.timezone("America/Mexico_City"))
    print(local_dt)
    data = post.model_dump()
    data["post_id"] = get_next_post_id()
    data["id_usuario"] = user_id
    data["fechaPublicacion"] = local_dt
    posts_collection.insert_one(data)
    return {"message": "Post created", "post_id": data["post_id"]}

@router.post("/upload-image")
async def upload_image(file: UploadFile = File(...),
    user_id: str = Depends(get_current_user)):
    try:
        result = cloudinary.uploader.upload(file.file)
        return {
            "url": result["secure_url"],
            "public_id": result["public_id"]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/posts")
def get_all_posts(user_id: str = Depends(get_current_user)):  # protegida también
    posts = list(posts_collection.find())
    return [
        {
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p["url_media"],
            "fechaPublicacion": convert_to_local(p["fechaPublicacion"]),
        }
        for p in posts
    ]


@router.get("/posts/user/{id_usuario}")
def get_user_posts(id_usuario: str, _: str = Depends(get_current_user)):
    posts = list(posts_collection.find({"id_usuario": id_usuario}))
    return [
        {
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p["url_media"],
            "fechaPublicacion": convert_to_local(p["fechaPublicacion"]),
        }
        for p in posts
    ]


@router.delete("/posts/{post_id}")
def delete_post(post_id: str, user_id: str = Depends(get_current_user)):
    post = posts_collection.find_one({"post_id": post_id})
    if not post:
        raise HTTPException(status_code=404, detail="Post not found")

    if str(post["id_usuario"]) != user_id:
        raise HTTPException(status_code=403, detail="No puedes borrar este post")

    posts_collection.delete_one({"post_id": post_id})
    return {"message": f"Post with ID '{post_id}' deleted."}


@router.get("/posts/recent")
def get_recent_posts(user_id: str = Depends(get_current_user)):
    now = datetime.now(timezone.utc)
    one_week_ago = now - timedelta(days=7)

    recent_posts = list(posts_collection.find({
        "fechaPublicacion": {"$gte": one_week_ago}
    }).sort("fechaPublicacion", -1))

    # Agrupar por (id_usuario, fechaPublicacion)
    from collections import defaultdict
    grouped = defaultdict(list)
    procesados = set()

    for post in recent_posts:
        user = str(post["id_usuario"])
        fecha_sin_hora = post["fechaPublicacion"].date()
        description = post["descripcion"]  
        grouped[(user, fecha_sin_hora, description)].append(post)

    rpc_comments = CommentCountRpcClient()
    rpc_likes = LikesCountRpcClient()

    result = []
    for (user_id, fecha_str, descripcion), posts in grouped.items():
        clave_grupo = (user_id, fecha_str, descripcion)
        if clave_grupo in procesados:
            continue
        procesados.add(clave_grupo)
        post_principal = posts[0]  # Usamos la descripción de este

        imagenes = []
        total_likes = 0
        total_comentarios = 0

        for p in posts:
            imagenes.append(p["url_media"])

            # RPC por cada imagen
            resp_c = rpc_comments.get_comment_count(str(p["post_id"]))
            total_comentarios += resp_c.get("count", 0)

            resp_l = rpc_likes.get_likes_count(str(p["post_id"]))
            total_likes += resp_l.get("like_count", 0)

        result.append({
            "post_id": post_principal["post_id"],
            "id_usuario": user_id,
            "descripcion": descripcion,
            "fechaPublicacion": convert_to_local(post_principal["fechaPublicacion"]),
            "imagenes": imagenes,
            "comentarios": total_comentarios,
            "likes": total_likes
        })

    return result


@router.get("/posts/{post_id}/comments")
def get_post_comments(post_id: str, user_id: str = Depends(get_current_user)):
    rpc = CommentListRpcClient()
    response = rpc.get_comments(str(post_id))

    if response is None or "comments" not in response:
        raise HTTPException(status_code=404, detail="No comments found or RPC failed")

    return {
        "post_id": str(post_id),
        "comments": response["comments"]
    }

@router.get("/posts/{post_id}/likes-count")
def get_post_likes_count(post_id: str, user_id: str = Depends(get_current_user)):
    rpc = LikesCountRpcClient()
    response = rpc.get_likes_count(post_id)

    if response is None or "like_count" not in response:
        raise HTTPException(status_code=404, detail="No likes found or RPC failed")

    return {
        "post_id": post_id,
        "like_count": response["like_count"]
    }
