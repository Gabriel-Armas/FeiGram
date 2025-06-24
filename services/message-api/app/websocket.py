import json
from fastapi import WebSocket, WebSocketDisconnect
from app.db import messages_collection
from app.models import Message, StoredMessage, message_to_dict
from datetime import datetime, timezone
from typing import Dict
from pymongo import DESCENDING
from bson import ObjectId

active_connections: Dict[str, WebSocket] = {}

def json_safe(obj):
    if isinstance(obj, datetime):
        return obj.isoformat()
    if isinstance(obj, ObjectId):
        return str(obj)
    raise TypeError(f"Object of type {type(obj).__name__} is not JSON serializable")

def get_chat_history(user_a: str, user_b: str, limit: int = 50):
    query = {
        "$or": [
            {"from": user_a, "to": user_b},
            {"from": user_b, "to": user_a}
        ]
    }
    messages = messages_collection.find(query).sort("timestamp", DESCENDING).limit(limit)
    
    history = []
    for msg in messages:
        msg["_id"] = str(msg["_id"])
        if isinstance(msg["timestamp"], datetime):
            msg["timestamp"] = msg["timestamp"].isoformat()
        history.append(msg)

    return history[::-1]

def get_contacts(user_id: str):
    pipeline = [
        {
            "$match": {
                "$or": [
                    {"from": user_id},
                    {"to": user_id}
                ]
            }
        },
        {
            "$project": {
                "contact_id": {
                    "$cond": [
                        {"$eq": ["$from", user_id]},
                        "$to",
                        "$from"
                    ]
                }
            }
        },
        {
            "$group": {
                "_id": "$contact_id"
            }
        }
    ]
    result = messages_collection.aggregate(pipeline)
    return [doc["_id"] for doc in result]

async def connect_user(user_id: str, websocket: WebSocket):
    active_connections[user_id] = websocket

async def disconnect_user(user_id: str):
    if user_id in active_connections:
        del active_connections[user_id]

async def handle_messages(user_id: str, websocket: WebSocket):
    await connect_user(user_id, websocket)
    try:
        while True:
            data = await websocket.receive_json()
            msg_type = data.get("type")

            if msg_type == "start_chat":
                other_user = data.get("with")
                history = get_chat_history(user_id, other_user)
                await websocket.send_text(json.dumps({
                    "type": "history",
                    "messages": history
                }, default=json_safe))
                continue

            if msg_type == "get_contacts":
                contacts = get_contacts(user_id)
                await websocket.send_text(json.dumps({
                    "type": "contacts",
                    "contacts": contacts
                }, default=json_safe))
                continue

            if "to" in data and "content" in data:
                incoming_msg = Message(**data)

                stored_msg = StoredMessage(
                    from_user=user_id,
                    to=incoming_msg.to,
                    content=incoming_msg.content,
                    timestamp=datetime.now(timezone.utc).isoformat()
                )

                messages_collection.insert_one(message_to_dict(stored_msg))

                if incoming_msg.to in active_connections:
                    await active_connections[incoming_msg.to].send_text(
                        json.dumps(stored_msg.model_dump(), default=json_safe)
                    )

    except WebSocketDisconnect:
        await disconnect_user(user_id)
