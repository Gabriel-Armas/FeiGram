from fastapi import APIRouter
from db import driver
from schemas import UserCreate

router = APIRouter()

@router.post("/user")
def create_user(user: UserCreate):
    with driver.session() as session:
        session.run("""
            MERGE (u:User {id: $id})
        """, id=user.id)
    return {"message": f"User with ID '{user.id}' created."}

@router.post("/follow/{follower}/{followed}")
def follow_user(follower: str, followed: str):
    with driver.session() as session:
        session.run("""
            MATCH (a:User {id: $from_user})
            MATCH (b:User {id: $to_user})
            MERGE (a)-[:FOLLOWS]->(b)
        """, from_user=follower, to_user=followed)
    return {"message": f"{follower} now follows {followed}"}

@router.get("/followers/{user_id}")
def get_followers(user_id: str):
    with driver.session() as session:
        result = session.run("""
            MATCH (a:User)-[:FOLLOWS]->(b:User {id: $id})
            RETURN a.id AS follower_id
        """, id=user_id)
        return {"followers": [r["follower_id"] for r in result]}
