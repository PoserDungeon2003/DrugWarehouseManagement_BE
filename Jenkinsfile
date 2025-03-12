pipeline {
    agent any

    environment {
        DOCKER_COMPOSE_PATH = "docker-compose.yml"
        API_SERVICE = "api"
    }

    stages {
        stage('Checkout Code') {
            steps {
                git branch: 'main', url: 'git@github.com:PoserDungeon2003/DrugWarehouseManagement_BE.git'
            }
        }

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
