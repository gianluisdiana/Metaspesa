FROM node:22-alpine AS builder

WORKDIR /client

COPY package.json package-lock.json ./

RUN npm ci

COPY . .

RUN npm run generate-proto

RUN npm run build

FROM node:22-alpine AS runner

WORKDIR /client

ENV NODE_ENV=production

COPY --from=builder /client/package.json ./
COPY --from=builder /client/node_modules ./node_modules
COPY --from=builder /client/.next ./.next
COPY --from=builder /client/public ./public
COPY --from=builder /client/next.config.* ./
COPY --from=builder /client/src/infrastructure/protos ./src/infrastructure/protos

ENTRYPOINT ["npm", "run", "start"]