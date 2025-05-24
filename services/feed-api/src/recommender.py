from motor.motor_asyncio import AsyncIOMotorDatabase

async def get_recommendations(user_id: str, db: AsyncIOMotorDatabase):
    user = await db.users.find_one({"_id": user_id})
    if not user:
        return []

    liked_posts = await db.likes.find({"user_id": user_id}).to_list(length=None)
    liked_post_ids = [like["post_id"] for like in liked_posts]

    followed_users = await db.follows.find({"follower_id": user_id}).to_list(length=None)
    followed_user_ids = [follow["followed_id"] for follow in followed_users]

    query = {
        "$or": [
            {"_id": {"$in": liked_post_ids}},
            {"user_id": {"$in": followed_user_ids}}
        ]
    }

    posts = await db.posts.find(query).sort("created_at", -1).to_list(length=50)
    for post in posts:
        post["_id"] = str(post["_id"])
    return posts