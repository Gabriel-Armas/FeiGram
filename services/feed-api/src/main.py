from fastapi import FastAPI
from src.rabbit_consumer import start_feed_consumer_thread
from src.routes import router
from src.rabbit_client import setup_rabbitmq

app = FastAPI(title="Feed API")

setup_rabbitmq()
start_feed_consumer_thread()
app.include_router(router)