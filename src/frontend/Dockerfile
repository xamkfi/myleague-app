FROM node:22-alpine
EXPOSE 5173
WORKDIR /app

COPY package.json pnpm-lock.yaml .

RUN npm install -g pnpm
RUN pnpm install

COPY . .

CMD ["pnpm", "dev"]