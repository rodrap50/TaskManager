#!/bin/sh
exec s6-setuidgid appuser nginx -g "daemon off;" -c /etc/nginx/nginx.conf
