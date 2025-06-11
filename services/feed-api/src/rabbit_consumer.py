import pika
import json
import threading
import asyncio
from recommender import get_recommendations

### 🐰 Handler de recomendaciones
def handle_feed_recommendation(ch, method, props, body):
    request = json.loads(body)
    user_id = request["user_id"]
    skip = request.get("skip", 0)
    limit = request.get("limit", 10)

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    posts = loop.run_until_complete(get_recommendations(user_id, skip, limit))
    loop.close()

    response_body = json.dumps({ "posts": posts })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response_body
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

### 🐹 Handler de likes
def handle_likes_request(ch, method, props, body):
    request = json.loads(body)
    user_id = request["user_id"]

    # Aquí pondrías la lógica real, pero por ahora:
    liked_post_ids = ["123", "456"]

    response_body = json.dumps({
        "liked_post_ids": liked_post_ids
    })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response_body
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

### 🐱 Handler de follows
def handle_follow_request(ch, method, props, body):
    request = json.loads(body)
    user_id = request["user_id"]

    # Lógica real futura aquí uwu
    followed_user_ids = ["user_a", "user_b"]

    response_body = json.dumps({
        "followed_user_ids": followed_user_ids
    })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response_body
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

def consume_feed_requests():
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
    channel = connection.channel()

    channel.queue_declare(queue='feed-post-requests')
    channel.basic_consume(queue='feed-post-requests', on_message_callback=handle_feed_recommendation)

    channel.queue_declare(queue='likes-requests')
    channel.basic_consume(queue='likes-requests', on_message_callback=handle_likes_request)

    channel.queue_declare(queue='follow-requests')
    channel.basic_consume(queue='follow-requests', on_message_callback=handle_follow_request)

    channel.basic_qos(prefetch_count=1)
    print("✨ RabbitMQ escuchando en todas las colas uwu")
    channel.start_consuming()

def start_feed_consumer_thread():
    thread = threading.Thread(target=consume_feed_requests, daemon=True)
    thread.start()