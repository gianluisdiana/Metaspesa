FROM node:24.16-alpine AS builder

WORKDIR /client

RUN corepack enable

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

RUN pnpm install --frozen-lockfile

COPY . .

RUN pnpm run generate-proto

RUN pnpm run build

FROM node:24.16-alpine AS runner

WORKDIR /client

ENV NODE_ENV=production

RUN corepack enable

COPY --from=builder /client/package.json ./
COPY --from=builder /client/pnpm-lock.yaml ./
COPY --from=builder /client/pnpm-workspace.yaml ./
COPY --from=builder /client/node_modules ./node_modules
COPY --from=builder /client/.next ./.next
COPY --from=builder /client/public ./public
COPY --from=builder /client/next.config.* ./
COPY --from=builder /client/src/infrastructure/protos ./src/infrastructure/protos

ENTRYPOINT ["pnpm", "run", "start"]