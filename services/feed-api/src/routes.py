from fastapi import APIRouter
from src.recommender import get_recommendations

router = APIRouter()

@router.get("/posts/recommendations")
async def recommendations(user_id: str, skip: int = 0, limit: int = 10):
    posts = await get_recommendations(user_id, skip, limit)
    return {"posts": posts}