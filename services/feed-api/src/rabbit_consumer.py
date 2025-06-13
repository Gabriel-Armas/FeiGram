from src.recommender import get_recommendations
import pika, json, asyncio
import threading

def handle_feed_recommendation(ch, method, props, body):
    request = json.loads(body)
    user_id = request["user_id"]
    skip = request.get("skip", 0)
    limit = request.get("limit", 10)

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        posts = loop.run_until_complete(get_recommendations(user_id, skip, limit))
    finally:
        loop.close()

    response_body = json.dumps({ "posts": posts })

    ch.basic_publish(
        exchange='',
        routing_key=props.reply_to,
        properties=pika.BasicProperties(correlation_id=props.correlation_id),
        body=response_body
    )
    ch.basic_ack(delivery_tag=method.delivery_tag)

def consume_feed_requests():
    connection = pika.BlockingConnection(pika.ConnectionParameters('rabbitmq'))
    channel = connection.channel()
    
    channel.queue_declare(
        queue='feed_recommendation_requests',
        durable=True,
        exclusive=False,
        auto_delete=False
    )

    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='feed_recommendation_requests', on_message_callback=handle_feed_recommendation)

    channel.start_consuming()

def start_feed_consumer_thread():
    thread = threading.Thread(target=consume_feed_requests, daemon=True)
    thread.start()