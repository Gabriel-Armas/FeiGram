from fastapi import FastAPI
from contextlib import asynccontextmanager
from src.rabbit_client import init_rabbitmq
from src.routes import router as likes_router

@asynccontextmanager
async def lifespan(app: FastAPI):
    await init_rabbitmq()
    yield

app = FastAPI(lifespan=lifespan)
app.include_router(likes_router)