from motor.motor_asyncio import AsyncIOMotorDatabase

async def get_recommendations(user_id: str, db):
    liked_post_ids = await db.likes.find({"user_id": user_id}).to_list(length=100)
    followed_user_ids = await db.follows.find({"follower_id": user_id}).to_list(length=100)
    
    posts_cursor = db.posts.find({})
    posts = await posts_cursor.to_list(length=100)

    filtered_posts = [
        post for post in posts
        if post["user_id"] in [f["followed_id"] for f in followed_user_ids]
        or post["_id"] in [like["post_id"] for like in liked_post_ids]
    ]

    if not filtered_posts:
        filtered_posts = sorted(posts, key=lambda p: p.get("likes_count", 0), reverse=True)

    return filtered_posts[:10]
