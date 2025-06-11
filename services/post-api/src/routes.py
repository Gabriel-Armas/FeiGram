from datetime import datetime, timedelta, timezone
from fastapi import APIRouter, HTTPException, Depends
from src.db import posts_collection, get_next_post_id
from src.schemas import PostCreate
from bson.objectid import ObjectId
from src.auth import get_current_user

router = APIRouter()

@router.post("/posts")
def create_post(post: PostCreate, user_id: str = Depends(get_current_user)):
    data = post.model_dump()
    data["post_id"] = get_next_post_id()
    data["id_usuario"] = user_id
    data["fechaPublicacion"] = datetime.now(timezone.utc)
    result = posts_collection.insert_one(data)
    return {"message": "Post created", "post_id": data["post_id"]}


@router.get("/posts")
def get_all_posts(user_id: str = Depends(get_current_user)):  # protegida también
    posts = list(posts_collection.find())
    return [
        {
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p["url_media"],
            "fechaPublicacion": p["fechaPublicacion"],
        }
        for p in posts
    ]


@router.get("/posts/user/{id_usuario}")
def get_user_posts(id_usuario: int, _: str = Depends(get_current_user)):  # Solo valida token
    posts = list(posts_collection.find({"id_usuario": id_usuario}))
    return [
        {
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p["url_media"],
            "fechaPublicacion": p["fechaPublicacion"],
        }
        for p in posts
    ]


@router.delete("/posts/{post_id}")
def delete_post(post_id: int, user_id: str = Depends(get_current_user)):
    post = posts_collection.find_one({"post_id": post_id})
    if not post:
        raise HTTPException(status_code=404, detail="Post not found")

    if str(post["id_usuario"]) != user_id:
        raise HTTPException(status_code=403, detail="No puedes borrar este post")

    posts_collection.delete_one({"post_id": post_id})
    return {"message": f"Post with ID '{post_id}' deleted."}


@router.get("/posts/recent")
def get_recent_posts(user_id: str = Depends(get_current_user)):  # protegida
    now = datetime.now(timezone.utc)
    one_week_ago = now - timedelta(days=7)

    recent_posts = list(posts_collection.find({
        "fechaPublicacion": {"$gte": one_week_ago}
    }))

    return [
        {
            "post_id": p["post_id"],
            "id_usuario": p["id_usuario"],
            "descripcion": p["descripcion"],
            "url_media": p["url_media"],
            "fechaPublicacion": p["fechaPublicacion"],
        }
        for p in recent_posts
    ]