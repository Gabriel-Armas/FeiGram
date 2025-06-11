from fastapi import FastAPI
from src.rabbit_consumer import start_feed_consumer_thread
from src.routes import router

app = FastAPI(title="Feed API")
start_feed_consumer_thread()
app.include_router(router)