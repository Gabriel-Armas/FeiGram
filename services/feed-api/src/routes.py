from fastapi import APIRouter, Depends
from src.recommender import get_recommendations, get_likes_by_user, get_follows_by_user
from src.auth import get_current_user  # Asegúrate de tener esta importación

router = APIRouter()

@router.get("/posts/recommendations")
async def recommendations(user_id: str = Depends(get_current_user), skip: int = 0, limit: int = 10):
    posts = await get_recommendations(user_id, skip, limit)
    return {"posts": posts}

@router.get("/likes/user")
async def get_user_likes(user_id: str = Depends(get_current_user)):
    liked_post_ids = await get_likes_by_user(user_id)
    return {"liked_post_ids": liked_post_ids}

@router.get("/follows/user")
async def get_user_following(user_id: str = Depends(get_current_user)):
    followed_user_ids = await get_follows_by_user(user_id)
    return {"followed_user_ids": followed_user_ids}
