from fastapi import FastAPI
from contextlib import asynccontextmanager
from routes import router
from rabbit_consumer import start_consumer_in_thread

@asynccontextmanager
async def lifespan(app: FastAPI):
    start_consumer_in_thread()
    yield

app = FastAPI(title="Follow API", lifespan=lifespan)
app.include_router(router)
