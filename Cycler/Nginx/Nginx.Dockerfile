FROM nginx:latest

COPY nginx.conf /etc/nginx/nginx.conf
COPY localhost.crt /etc/ssl/certs/localhost.crt
COPY localhost.key /etc/ssl/private/localhost.key
COPY qRQ3JRWylkN8xLymqbS4eGaoxuSRKwKHoZsYtI8zO14 /etc/ssl/private/.well-known/acme-challenge/qRQ3JRWylkN8xLymqbS4eGaoxuSRKwKHoZsYtI8zO14