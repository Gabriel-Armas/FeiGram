from db import db

async def update_recommendations_based_on_like(user_id: str, post_id: str):
    await db.likes.insert_one({"user_id": user_id, "post_id": post_id})

async def update_recommendations_based_on_follow(user_id: str, followed_id: str):
    await db.follows.insert_one({"follower_id": user_id, "followed_id": followed_id})