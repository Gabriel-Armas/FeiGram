import os
from pymongo import MongoClient
from pymongo import ReturnDocument

MONGODB_URI = os.environ.get("MONGODB_URI", "mongodb://root:examplepassword@mongo:27017")
MONGODB_DBNAME = os.environ.get("MONGODB_DBNAME", "feigram")

client = MongoClient(MONGODB_URI)
db = client[MONGODB_DBNAME]
posts_collection = db["posts"]

if not db.counters.find_one({"_id": "post_id"}):
    db.counters.insert_one({"_id": "post_id", "sequence_value": 0})

def get_next_post_id():
    counter = db.counters.find_one_and_update(
        {"_id": "post_id"},
        {"$inc": {"sequence_value": 1}},
        return_document=ReturnDocument.AFTER
    )
    return counter["sequence_value"]
