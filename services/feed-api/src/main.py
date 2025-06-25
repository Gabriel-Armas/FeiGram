from fastapi import FastAPI
from contextlib import asynccontextmanager
from src.rabbit_client import init_rabbitmq
from src.rabbit_consumer import start_feed_consumer_thread
from src.routes import router as router

@asynccontextmanager
async def lifespan(app: FastAPI):
    await init_rabbitmq()
    start_feed_consumer_thread()
    yield

app = FastAPI(lifespan=lifespan)

app.include_router(router)