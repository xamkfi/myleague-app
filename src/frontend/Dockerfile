FROM node:18-alpine

WORKDIR /app

COPY package.json

RUN pnpm install

COPY . .

EXPOSE 3000

CMD ["pnpm", "dev"]