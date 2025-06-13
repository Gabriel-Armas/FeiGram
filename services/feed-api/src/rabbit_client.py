import aio_pika
import uuid
import asyncio
import json

class RabbitMQClient:
    def __init__(self, loop, amqp_url: str, request_queue: str):
        self.loop = loop
        self.amqp_url = amqp_url
        self.request_queue = request_queue
        self.connection = None
        self.channel = None
        self.callback_queue = None
        self.response_futures = {}

    async def connect(self):
        print("Conectando a RabbitMQ de forma asíncrona~")
        self.connection = await aio_pika.connect_robust(self.amqp_url, loop=self.loop)
        self.channel = await self.connection.channel()

        self.callback_queue = await self.channel.declare_queue(exclusive=True)

        await self.callback_queue.consume(self._on_response)
        print("¡Conectado a RabbitMQ correctamente~!")

    async def _on_response(self, message: aio_pika.IncomingMessage):
        async with message.process():
            correlation_id = message.correlation_id
            if correlation_id in self.response_futures:
                future = self.response_futures.pop(correlation_id)
                future.set_result(message.body)

    async def call_rpc(self, payload: dict) -> dict:
        correlation_id = str(uuid.uuid4())
        future = self.loop.create_future()
        self.response_futures[correlation_id] = future

        message = aio_pika.Message(
            body=json.dumps(payload).encode(),
            correlation_id=correlation_id,
            reply_to=self.callback_queue.name
        )

        await self.channel.default_exchange.publish(
            message,
            routing_key=self.request_queue
        )

        response = await future
        return json.loads(response)

likes_client: RabbitMQClient = None
posts_client: RabbitMQClient = None
follows_client: RabbitMQClient = None

async def request_likes_by_user(user_id: str):
    payload = {"action": "get_likes_by_user", "user_id": user_id}
    return await likes_client.call_rpc(payload)

async def request_followed_users_by_user(user_id: str):
    payload = {"action": "get_follows_by_user", "user_id": user_id}
    return await follows_client.call_rpc(payload)

async def request_posts_from_post_service(skip: int = 0, limit: int = 10):
    payload = {"action": "get_recommendations", "skip": skip, "limit": limit}
    return await posts_client.call_rpc(payload)


async def init_rabbitmq():
    global likes_client, posts_client, follows_client
    loop = asyncio.get_event_loop()
    likes_client = RabbitMQClient(loop, "amqp://guest:guest@rabbitmq:5672/", "likes_queue")
    await likes_client.connect()

    posts_client = RabbitMQClient(loop, "amqp://guest:guest@rabbitmq:5672/", "get-feed-posts")
    await posts_client.connect()

    follows_client = RabbitMQClient(loop, "amqp://guest:guest@rabbitmq:5672/", "follows_queue")
    await follows_client.connect()