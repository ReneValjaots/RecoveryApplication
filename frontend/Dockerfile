FROM node:22.11-alpine3.19 as builder
RUN npm install -g pnpm

WORKDIR /app

COPY ./package.json ./pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

FROM node:22.11-alpine3.19

WORKDIR /app

COPY --from=builder /app/.output ./.output

EXPOSE 3000
CMD [ "node", ".output/server/index.mjs" ]
