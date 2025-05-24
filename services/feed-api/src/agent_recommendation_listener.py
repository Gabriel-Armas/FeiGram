import pika
import json
import asyncio
from recommendation_logic import update_recommendations_based_on_like, update_recommendations_based_on_follow

import asyncio

loop = asyncio.new_event_loop()
asyncio.set_event_loop(loop)

def callback(ch, method, properties, body):
    data = json.loads(body)

    if method.routing_key == "user.liked.post":
        loop.create_task(update_recommendations_based_on_like(data["user_id"], data["post_id"]))
    elif method.routing_key == "user.followed.user":
        loop.create_task(update_recommendations_based_on_follow(data["user_id"], data["followed_id"]))


def start_agent_listener():
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
    channel = connection.channel()

    channel.exchange_declare(exchange='feigram_events', exchange_type='topic')
    channel.queue_declare(queue='recommendation_agent_queue')
    channel.queue_bind(exchange='feigram_events', queue='recommendation_agent_queue', routing_key='user.*')

    channel.basic_consume(queue='recommendation_agent_queue', on_message_callback=callback, auto_ack=True)

    channel.start_consuming()