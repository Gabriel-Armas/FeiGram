from fastapi import FastAPI
from routes import router
from contextlib import asynccontextmanager
from rabbit_consumer import start_consumer_thread

@asynccontextmanager
async def lifespan(app: FastAPI):
    print("🌸 Iniciando Feigram... levantando consumidor de likes~")
    start_consumer_thread()
    
    yield

    print("🌙 Apagando Feigram... ¡nos vemos en la próxima temporada~!")

app = FastAPI(lifespan=lifespan)
app.include_router(router)
