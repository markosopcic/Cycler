FROM nginx:latest

COPY nginx.conf /etc/nginx/nginx.conf
COPY localhost.crt /etc/ssl/certs/localhost.crt
COPY localhost.key /etc/ssl/private/localhost.key
COPY q27Pgeo6vV4dgtS4KxF-QYbOXXX0anj22RSpFWh-NSw /etc/ssl/private/sslfile