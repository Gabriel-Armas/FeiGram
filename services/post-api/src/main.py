from fastapi import FastAPI
from src.routes import router
from src.upload_routes import router as upload_router

app = FastAPI(title="Post API")
app.include_router(router)
app.include_router(upload_router)
