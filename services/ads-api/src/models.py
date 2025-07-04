from pydantic import BaseModel
from typing import Optional

class Ad(BaseModel):
    brandName: str
    publicationDate: str
    urlMedia: str
    urlSite: str
    description: str
    amount: int