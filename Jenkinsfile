pipeline {
    agent any

    environment {
        DOCKER_COMPOSE_PATH = "docker-compose.yml"
        API_SERVICE = "api"
    }

    stages {
        stage('Build API Service') {
            steps {
                sh 'docker compose -f $DOCKER_COMPOSE_PATH build $API_SERVICE'
            }
        }

        stage('Restart Service') {
            steps {
                sh 'docker compose -f $DOCKER_COMPOSE_PATH up -d $API_SERVICE'
            }
        }
    }
}
