from src.recommender import get_recommendations
import pika, json, asyncio

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