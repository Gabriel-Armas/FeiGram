import json
import threading
import time
import pika
from db import driver

def handle_create_user_event(ch, method, properties, body):
    message = json.loads(body.decode())
    print(f"Evento de creación de usuario recibido: {message}")

    user_id = message.get("UserId")
    if not user_id:
        print("userId faltante en el mensaje. Ignorado.")
        ch.basic_ack(delivery_tag=method.delivery_tag)
        return

    with driver.session() as session:
        session.run("""
            MERGE (u:User {id: $id})
        """, {"id": user_id})

    print(f"Nodo User creado o confirmado: {user_id}")
    ch.basic_ack(delivery_tag=method.delivery_tag)

def handle_get_followers_count(ch, method, props, body):
    request = json.loads(body.decode())
    profile_id = request.get("profileId")
    print(f"Solicitud de conteo de seguidores para: {profile_id}")

    with driver.session() as session:
        result = session.run("""
            MATCH (p:Profile {id: $id})<-[:FOLLOWS]-(:Profile)
            RETURN count(*) as followersCount
        """, {"id": profile_id})
        count = result.single()["followersCount"]

    response = json.dumps({
        "profileId": profile_id,
        "followerCount": count
    })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)


def handle_get_following_profiles(ch, method, props, body):
    request = json.loads(body.decode())
    profile_id = request.get("profileId")
    print(f"Solicitud de perfiles seguidos por: {profile_id}")

    with driver.session() as session:
        result = session.run("""
            MATCH (p:Profile {id: $id})-[:FOLLOWS]->(followed:Profile)
            RETURN followed.id AS id
        """, {"id": profile_id})

        following_ids = [record["id"] for record in result]

    response = json.dumps({
        "followingIds": following_ids
    })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

def consume():
    connection = None
    while not connection:
        try:
            print("Intentando conectar a RabbitMQ...")
            connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
        except pika.exceptions.AMQPConnectionError:
            print("RabbitMQ no disponible. Reintentando en 5 segundos...")
            time.sleep(5)

    channel = connection.channel()

    channel.queue_declare(queue='get-followers-count')
    channel.queue_declare(queue='get-following-profiles')

    channel.queue_declare(queue='create-follow-user-queue')

    channel.basic_qos(prefetch_count=1)

    channel.basic_consume(queue='get-followers-count', on_message_callback=handle_get_followers_count)
    channel.basic_consume(queue='get-following-profiles', on_message_callback=handle_get_following_profiles)

    channel.basic_consume(queue='create-follow-user-queue', on_message_callback=handle_create_user_event)

    print("Escuchando solicitudes RPC y eventos de creación de usuario...")
    channel.start_consuming()

def start_consumer_in_thread():
    thread = threading.Thread(target=consume, daemon=True)
    thread.start()
