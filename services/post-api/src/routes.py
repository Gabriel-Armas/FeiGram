from fastapi import APIRouter, HTTPException
from src.db import posts_collection, get_next_post_id
from src.schemas import PostCreate
from bson.objectid import ObjectId

router = APIRouter()

@router.post("/posts")
def create_post(post: PostCreate):
    data = post.model_dump()
    data["post_id"] = get_next_post_id()  # ID incremental
    result = posts_collection.insert_one(data)
    return {"message": "Post created", "post_id": data["post_id"]}

@router.get("/posts")
def get_all_posts():
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
def get_user_posts(id_usuario: int):
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
def delete_post(post_id: int):
    result = posts_collection.delete_one({"post_id": post_id})
    if result.deleted_count == 1:
        return {"message": f"Post with ID '{post_id}' deleted."}
    else:
        raise HTTPException(status_code=404, detail="Post not found")
