from fastapi import FastAPI
from src.routes import router
from src.upload_routes import router as upload_router
from src.rabbit_consumer import start_consumer_in_thread

app = FastAPI(title="Post API")
start_consumer_in_thread()
app.include_router(router)
app.include_router(upload_router)
