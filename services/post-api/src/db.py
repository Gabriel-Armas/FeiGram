import os
from pymongo import MongoClient
from pymongo import ReturnDocument


# No es necesario usar load_dotenv() en producción con Docker
MONGODB_URI = os.environ.get("MONGODB_URI", "mongodb://localhost:27017")
MONGODB_DBNAME = os.environ.get("MONGODB_DBNAME", "feigram")

client = MongoClient(MONGODB_URI)
db = client[MONGODB_DBNAME]
posts_collection = db["posts"]

# Inicializa la colección de contadores si no existe
if not db.counters.find_one({"_id": "post_id"}):
    db.counters.insert_one({"_id": "post_id", "sequence_value": 0})

# Función para obtener el siguiente ID incremental
def get_next_post_id():
    counter = db.counters.find_one_and_update(
        {"_id": "post_id"},
        {"$inc": {"sequence_value": 1}},
        return_document=ReturnDocument.AFTER
    )
    return counter["sequence_value"]
