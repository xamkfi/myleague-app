FROM node:22-alpine
EXPOSE 5173
WORKDIR /app

COPY package.json pnpm-lock.yaml ./

RUN npm install -g pnpm@10.11.0
RUN pnpm install --frozen-lockfile

COPY . .

CMD ["pnpm", "dev", "--mode", "development"]