import json
import threading
import time
import pika

def handle_like(ch, method, props, body):
    like = json.loads(body)
    print(f"🌸 Nuevo like recibido: Usuario {like['UserId']} le dio like al post {like['PostId']}")

    ch.basic_ack(delivery_tag=method.delivery_tag)

def consume_likes():
    connection = None
    while not connection:
        try:
            print("Intentando conectar a RabbitMQ para likes...")
            connection = pika.BlockingConnection(pika.ConnectionParameters(host='rabbitmq'))
        except pika.exceptions.AMQPConnectionError:
            print("RabbitMQ no disponible, reintentando en 5 segundos...")
            time.sleep(5)

    channel = connection.channel()

    channel.exchange_declare(exchange='feigram_events', exchange_type='topic')
    channel.queue_declare(queue='feed_likes_queue', durable=True)

    channel.queue_bind(exchange='feigram_events', queue='feed_likes_queue', routing_key='feigram.likes.created')

    channel.basic_qos(prefetch_count=1)
    channel.basic_consume(queue='feed_likes_queue', on_message_callback=handle_like)

    print("🐾 FeedApi está escuchando likes... ¡kawaii!")
    channel.start_consuming()

def start_consumer_thread():
    thread = threading.Thread(target=consume_likes, daemon=True)
    thread.start()
