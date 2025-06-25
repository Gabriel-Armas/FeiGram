from pydantic import BaseModel
from datetime import datetime
from typing import Optional

class Message(BaseModel):
    to: str
    content: str

class StoredMessage(BaseModel):
    from_user: str
    to: str
    content: str
    timestamp: datetime

def message_to_dict(message: StoredMessage) -> dict:
    return {
        "from": message.from_user,
        "to": message.to,
        "content": message.content,
        "timestamp": message.timestamp
    }
