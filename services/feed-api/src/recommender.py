from src.rabbit_client import request_likes_by_user, request_followed_users_by_user, request_posts_from_post_service

import json

import json

async def get_recommendations(user_id: str, skip: int = 0, limit: int = 10):
    liked_post_ids = await request_likes_by_user(user_id)
    # followed_user_ids = await request_followed_users_by_user(user_id)
    all_posts = await request_posts_from_post_service(skip=skip, limit=30)

    parsed_posts = []
    for post in all_posts:
        if isinstance(post, str):
            post = post.strip()
            if post:
                try:
                    post = json.loads(post)
                except json.JSONDecodeError:
                    continue
            else:
                continue
        parsed_posts.append(post)

    filtered_posts = [
        post for post in parsed_posts
        # if post.get("id_usuario") in followed_user_ids
        or post.get("post_id") in liked_post_ids
    ]

    if not filtered_posts:
        filtered_posts = sorted(parsed_posts, key=lambda p: p.get("likes_count", 0), reverse=True)

    return filtered_posts[:limit]

async def get_likes_by_user(user_id: str):
    response = await request_likes_by_user(user_id)
    return response.get('liked_post_ids', [])

async def get_follows_by_user(user_id: str):
    response = await request_followed_users_by_user(user_id)
    return response.get('followed_user_ids', [])