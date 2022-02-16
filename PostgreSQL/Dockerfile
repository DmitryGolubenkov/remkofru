FROM postgres:14.1
RUN localedef -i ru_RU -c -f UTF-8 -A /usr/share/locale/locale.alias ru_RU.UTF-8
ENV LANG ru_RU.utf8

COPY *.sql /docker-entrypoint-initdb.d/
#COPY docker-entrypoint.sh /

#RUN chmod +x /docker-entrypoint.sh
#USER postgres
#ENTRYPOINT ["/docker-entrypoint.sh"]

EXPOSE 5432
CMD ["postgres"]
