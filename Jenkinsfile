pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'pagina-bizu'
        DOCKER_TAG = "${env.BUILD_NUMBER}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                script {
                    // Build the solution
                    bat 'dotnet build "PaginaBizu.sln" --configuration Release'
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    // Run tests
                    bat 'dotnet test "PaginaBizu.Tests/PaginaBizu.Tests.csproj" --no-build --verbosity normal --configuration Release'
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    // Build Docker image
                    docker.build("${env.DOCKER_IMAGE}:${env.DOCKER_TAG}", "-f Dockerfile .")
                }
            }
        }
        
        stage('Push to GitHub') {
            when {
                branch 'main'  // Only push to GitHub when on main branch
            }
            steps {
                script {
                    withCredentials([
                        usernamePassword(
                            credentialsId: 'github-credentials',
                            usernameVariable: 'GIT_USERNAME',
                            passwordVariable: 'GIT_TOKEN'
                        )
                    ]) {
                        // Configure Git
                        bat 'git config --global user.email "jenkins@example.com"'
                        bat 'git config --global user.name "Jenkins"'
                        
                        // Push changes to GitHub
                        bat 'git remote set-url origin https://${GIT_TOKEN}@github.com/yourusername/ProyectoAnalisisDeSistemas.git'
                        bat 'git add .'
                        bat 'git commit -m "Jenkins build ${env.BUILD_NUMBER}" || echo "No changes to commit"'
                        bat 'git push origin HEAD:main'
                    }
                }
            }
        }
    }
    
    post {
        success {
            echo 'Pipeline completed successfully!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
