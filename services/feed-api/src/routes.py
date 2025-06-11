from fastapi import APIRouter
from db import db
from recommender import get_recommendations as get_recommendations_logic

router = APIRouter()

@router.get("/posts")
async def get_posts():
    posts_cursor = db.posts.find()
    posts = []
    async for post in posts_cursor:
        post["_id"] = str(post["_id"])
        posts.append(post)
    return posts

@router.get("/posts/recommendations")
async def get_recommendations(user_id: str, skip: int = 0, limit: int = 10):
    return await get_recommendations_logic(user_id, db, skip, limit)