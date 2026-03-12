FROM selenium/standalone-firefox:128.0-20260222

WORKDIR /app

COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

ENTRYPOINT [ "python", "./src/main.py" ]