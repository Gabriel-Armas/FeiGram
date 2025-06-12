from fastapi import FastAPI
from src.routes import router
from src.rabbit_consumer import start_feed_consumer_thread

app = FastAPI(title="Post API")
start_feed_consumer_thread()
app.include_router(router)
