from fastapi import FastAPI
from routes import router
# import threading

app = FastAPI()
app.include_router(router)

# def run_listener():
#     from agent_recommendation_listener import start_agent_listener
#     start_agent_listener()

# threading.Thread(target=run_listener, daemon=True).start()
