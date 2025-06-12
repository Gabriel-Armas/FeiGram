from fastapi import FastAPI
from routes import router

app = FastAPI(title="Follow API")
app.include_router(router)
