import pika
import json
import uuid
import asyncio
import threading

_connection = None
_channel = None
_callback_queue = None
_responses = {}
_lock = threading.Lock()

def setup_rabbitmq(host='rabbitmq'):
    global _connection, _channel, _callback_queue
    _connection = pika.BlockingConnection(pika.ConnectionParameters(host=host))
    _channel = _connection.channel()
    result = _channel.queue_declare(queue='', exclusive=True)
    _callback_queue = result.method.queue
    _channel.basic_consume(queue=_callback_queue, on_message_callback=_on_response, auto_ack=True)
    threading.Thread(target=_channel.start_consuming, daemon=True).start()

def _on_response(ch, method, props, body):
    with _lock:
        _responses[props.correlation_id] = body

async def call_rpc(queue_name: str, message: dict):
    correlation_id = str(uuid.uuid4())
    with _lock:
        _responses[correlation_id] = None

    _channel.basic_publish(
        exchange='',
        routing_key=queue_name,
        properties=pika.BasicProperties(
            reply_to=_callback_queue,
            correlation_id=correlation_id
        ),
        body=json.dumps(message)
    )

    while True:
        await asyncio.sleep(0.01)
        with _lock:
            if _responses[correlation_id] is not None:
                response_body = _responses.pop(correlation_id)
                return json.loads(response_body)

async def request_likes_by_user(user_id: str):
    response = await call_rpc('likes-requests', {'user_id': user_id})
    return response.get('liked_post_ids', [])

async def request_followed_users_by_user(user_id: str):
    response = await call_rpc('follow-requests', {'user_id': user_id})
    return response.get('followed_user_ids', [])

async def request_posts_from_post_service(skip: int = 0, limit: int = 30):
    response = await call_rpc('feed-post-requests', {'skip': skip, 'limit': limit})
    return response.get('posts', [])