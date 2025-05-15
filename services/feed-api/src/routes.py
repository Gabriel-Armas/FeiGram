from fastapi import APIRouter
from motor.motor_asyncio import AsyncIOMotorClient
import os
from dotenv import load_dotenv

load_dotenv()

router = APIRouter()

MONGO_URI = os.getenv("MONGO_URI")
MONGO_DB = os.getenv("MONGO_DB")

client = AsyncIOMotorClient(MONGO_URI)
db = client[MONGO_DB]

@router.get("/")
async def root():
    return {"message": "¡Konnichiwa, Yael-kun! Feigram API está ready~ 💖"}

@router.get("/posts")
async def get_posts():
    posts_cursor = db.posts.find()
    posts = []
    async for post in posts_cursor:
        post["_id"] = str(post["_id"])
        posts.append(post)
    return posts