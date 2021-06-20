@echo off 
docker build -t remkoffrontend -f .\RemkofFrontend\Dockerfile .
docker build -t remkofdatabase -f .\PostgreSQL\Dockerfile .\PostgreSQL\
docker-compose up -d