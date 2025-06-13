from fastapi import FastAPI
from routes import router
from rabbit_consumer import start_consumer_in_thread

start_consumer_in_thread()

app = FastAPI(title="Follow API")
app.include_router(router)
