# Step 1: Build the Angular app
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# Step 2: Serve the app with Nginx
FROM nginx:alpine

# Remove default Nginx website files completely
RUN rm -rf /usr/share/nginx/html/*

# CHANGED: Removed "/browser" from the end of the path
COPY --from=build /app/dist/ERP-Bancaire-Client /usr/share/nginx/html
# FIXED TYPO: Changed /etc/etc/nginx/... to /etc/nginx/...
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]