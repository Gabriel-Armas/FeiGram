from pymongo import MongoClient
import os

MONGO_URL = os.getenv("MONGO_URL", "mongodb://localhost:27017")
client = MongoClient(MONGO_URL)
db = client["message_db"]
messages_collection = db["messages"]
