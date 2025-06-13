from fastapi import APIRouter
from db import driver
from schemas import UserCreate

router = APIRouter()

@router.post("/user")
def create_user(user: UserCreate):
    try:
        with driver.session() as session:
            session.run("""
                MERGE (u:User {id: $id})
            """, id=user.id)
        return {"message": f"User with ID '{user.id}' created."}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=f"Neo4j error: {str(e)}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")

@router.post("/follow/{follower}/{followed}")
def follow_user(follower: str, followed: str):
    try:
        with driver.session() as session:
            session.run("""
                MATCH (a:User {id: $from_user})
                MATCH (b:User {id: $to_user})
                MERGE (a)-[:FOLLOWS]->(b)
            """, from_user=follower, to_user=followed)
        return {"message": f"{follower} now follows {followed}"}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/followers/{user_id}")
def get_followers(user_id: str):
    try:
        with driver.session() as session:
            result = session.run("""
                MATCH (a:User)-[:FOLLOWS]->(b:User {id: $id})
                RETURN a.id AS follower_id
            """, id=user_id)
            return {"followers": [r["follower_id"] for r in result]}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@router.get("/following/{user_id}")
def get_following(user_id: str):
    try:
        with driver.session() as session:
            result = session.run("""
                MATCH (a:User {id: $id})-[:FOLLOWS]->(b:User)
                RETURN b.id AS followed_id
            """, id=user_id)
            return {"following": [r["followed_id"] for r in result]}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@router.delete("/delete/{user_id}")
def delete_user(user_id: str):
    try:
        with driver.session() as session:
            session.run("""
                MATCH (u:User {id: $id})
                OPTIONAL MATCH (u)-[r1:FOLLOWS]->()
                OPTIONAL MATCH ()-[r2:FOLLOWS]->(u)
                DELETE r1, r2, u
            """, id=user_id)
        return {"message": f"User with ID '{user_id}' and all follow relationships deleted."}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@router.delete("/unfollow/{follower}/{followed}")
def unfollow_user(follower: str, followed: str):
    try:
        with driver.session() as session:
            session.run("""
                MATCH (a:User {id: $from_user})-[r:FOLLOWS]->(b:User {id: $to_user})
                DELETE r
            """, from_user=follower, to_user=followed)
        return {"message": f"{follower} has unfollowed {followed}"}
    except Neo4jError as e:
        raise HTTPException(status_code=500, detail=str(e))