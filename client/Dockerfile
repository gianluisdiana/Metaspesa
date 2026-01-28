FROM node:22-alpine AS builder

WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

RUN npm run generate-proto

RUN npm run build

FROM node:22-alpine AS runner

WORKDIR /src

ENV NODE_ENV=production
ENV PORT=8080

COPY --from=builder /src/package.json ./
COPY --from=builder /src/node_modules ./node_modules
COPY --from=builder /src/.next ./.next
COPY --from=builder /src/public ./public
COPY --from=builder /src/next.config.* ./
COPY --from=builder /src/infrastructure/protos ./infrastructure/protos

EXPOSE 8080

ENTRYPOINT ["npm", "run", "start"]